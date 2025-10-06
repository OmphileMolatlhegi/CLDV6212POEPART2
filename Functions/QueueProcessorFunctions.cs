using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetailOnlineFunctions.Functions
{
    internal class QueueProcessorFunctions
    {
        [Function("OrderNotifications_Processor")]
        public void OrderNotificationsProcessor(
      [QueueTrigger("%QUEUE_ORDER_NOTIFICATIONS%", Connection = "STORAGE_CONNECTION")] string message,
      FunctionContext ctx)
        {
            var log = ctx.GetLogger("OrderNotifications_Processor");
            log.LogInformation($"OrderNotifications message: {message}");
            // (Optional) write receipts, send emails, etc.
        }

        [Function("StockUpdates_Processor")]
        public void StockUpdatesProcessor(
            [QueueTrigger("%QUEUE_STOCK_UPDATES%", Connection = "STORAGE_CONNECTION")] string message,
            FunctionContext ctx)
        {
            var log = ctx.GetLogger("StockUpdates_Processor");
            log.LogInformation($"StockUpdates message: {message}");
            // (Optional) sync to reporting DB, etc.
        }
    }
}
