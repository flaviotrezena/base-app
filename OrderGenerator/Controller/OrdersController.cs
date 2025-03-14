using Microsoft.AspNetCore.Mvc;
using QuickFix.Fields;
using QuickFix.FIX44;
using System.Threading;


[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private static readonly object _lock = new object();
    private static ExecutionReport? _lastExecutionReport;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {

        if (!_orderService.isSessionConnected())
        {
            return StatusCode(504, "Session is not avaiable.");
        }


        _orderService.SendOrder(order);

        // Wait for ExecutionReport response
        await Task.Delay(1000);

        lock (_lock)
        {
            if (_lastExecutionReport != null)
            {
                return Ok(new { Message = ConvertMessageStatus(_lastExecutionReport.ExecType.Value.ToString()) });
            }
        }
        return StatusCode(504, "ExecutionReport not received in time");
    }

    private string ConvertMessageStatus(string value)
    {
        return ("0".Equals(value)) ? "Sucesso" : "Falha";
    }

    public static void OnExecutionReport(ExecutionReport report)
    {
        lock (_lock)
        {
            _lastExecutionReport = report;
        }
    }
}
