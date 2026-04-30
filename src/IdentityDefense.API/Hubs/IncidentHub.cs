using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IdentityDefense.API.Hubs;

[Authorize]
public class IncidentHub : Hub
{
}