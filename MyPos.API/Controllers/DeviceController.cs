using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPos.API.Application.DTOs;
using MyPos.API.Infrastructure.Data;

namespace MyPos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SUPERADMIN")]
    public class DeviceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DeviceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Pending requests list
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var list = await _context.DeviceApprovalRequests
                .Include(r => r.User)
                .Include(r => r.Client)
                .Where(r => r.Status == "PENDING")
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new PendingDeviceDto
                {
                    RequestId = r.RequestId,
                    UserId = r.UserId,
                    UserName = r.User.UserName,
                    FullName = r.User.FullName,
                    DeviceId = r.DeviceId,
                    Platform = r.Platform,
                    RequestedAt = r.RequestedAt,
                    ClientName = r.Client.ClientName
                })
                .ToListAsync();

            return Ok(list);
        }

        // Approve ya Reject
        [HttpPost("action")]
        public async Task<IActionResult> Action([FromBody] DeviceApproveRequest request)
        {
            var approval = await _context.DeviceApprovalRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RequestId == request.RequestId && r.Status == "PENDING");

            if (approval == null)
                return NotFound(new { message = "Request not found or already processed" });

            var action = request.Action.ToUpper();

            if (action == "APPROVE")
            {
                // Purana device hatao, naya register karo
                approval.User.RegisteredDeviceId = approval.DeviceId;
                approval.User.RegisteredPlatform = approval.Platform;
                approval.User.DeviceRegisteredAt = DateTime.UtcNow;
                approval.User.UpdatedAt = DateTime.UtcNow;

                approval.Status = "APPROVED";
                approval.ApprovedAt = DateTime.UtcNow;
                approval.Remarks = request.Remarks;
            }
            else if (action == "REJECT")
            {
                approval.Status = "REJECTED";
                approval.ApprovedAt = DateTime.UtcNow;
                approval.Remarks = request.Remarks;
            }
            else
            {
                return BadRequest(new { message = "Action must be APPROVE or REJECT" });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Device {action} successfully" });
        }
    }
}