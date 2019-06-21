using System;

using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Orders;

namespace QuantConnect
{
	class BootCampTask : QCAlgorithm
	{
		// BEGIN TASK 3
		// Order ticket for our stop loss
		private OrderTicket _stopLossTicket;

		// Datetime when stop loss or take profit was last hit
		private DateTime _lastLimitHitAt;
		// END TASK 3

		// BEGIN TASK 4
		// The highest close price our main asset has achieved since placing our market order
		private decimal _highestClose = -1m;
		// END TASK 4

		// BEGIN TASK 5
		// Order ticket for our take profit
		private OrderTicket _takeProfitTicket;
		// END TASK 5

		public override void Initialize()
		{
			SetStartDate(2018, 12, 1);
			SetEndDate(2019, 4, 1);
			SetCash(100000);

			AddSecurity(SecurityType.Equity, "SPY", Resolution.Daily);
		}

		public override void OnData(Slice slice)
		{
			// BEGIN TASK 7
			// Plot the asset price in a separate chart
			Plot("Data chart", "Asset price", Securities["SPY"].Close);
			// END TASK 7

			// BEGIN TASK 3
			// Check that at least 14 days (~2 weeks) have passed since we last hit our limit order
			if ((Time - _lastLimitHitAt).TotalDays < 14)
				return;
			// END TASK 3

			if (!Portfolio.Invested) {

				// Create market order for some units of SPY
				MarketOrder("SPY", 500);

				// Create stop loss through a stop market order
				StopMarketOrder("SPY", -500, 0.90m * Securities["SPY"].Close);

				// BEGIN TASK 3
				// _stopLossTicket = StopMarketOrder( "SPY", -500, 0.90m * Securities["SPY"].Close );
				// END TASK 3

				// BEGIN TASK 4
				// Store current price as the highest price
				_highestClose = Securities["SPY"].Close;
				// END TASK 4

				// BEGIN TASK 5
				// Create take profit through a limit order
				_takeProfitTicket = LimitOrder("SPY", -500, 1.10m * Securities["SPY"].Close);
				// END TASK 5

			} else {

				// BEGIN TASK 4
				// Update stop loss price if main asset has risen above its highest price
				if (Securities["SPY"].Close > _highestClose) {
					_stopLossTicket.Update(new UpdateOrderFields() { StopPrice = Securities["SPY"].Close * 0.90m });

					_highestClose = Securities["SPY"].Close;

					Debug("SL:" + Securities["SPY"].Close * 0.90m);
				}
				// END TASK 4

				// BEGIN TASK 7
				// Plot the current stop loss price
				Plot("Data chart", "Stop loss price", _stopLossTicket.Get(OrderField.StopPrice));

				// Plot the current take profit price
				Plot("Data chart", "Take profit price", _takeProfitTicket.Get(OrderField.LimitPrice));
				// END TASK 7
			}
		}

		public override void OnOrderEvent(OrderEvent orderEvent)
		{
			// BEGIN TASK 2
			// Only act on fills
			if (orderEvent.Status != OrderStatus.Filled)
				return;

			// Log order ID (can be extended to log more information)
			Debug(orderEvent.OrderId);
			// END TASK 2

			// BEGIN TASK 3
			// Check if we hit our stop loss
			if (_stopLossTicket != null && orderEvent.OrderId == _stopLossTicket.OrderId) {
				_lastLimitHitAt = Time;

				// BEGIN TASK 6
				// Cancel the take profit, we no longer need it
				Debug("Cancel:" + _takeProfitTicket.OrderId);
				_takeProfitTicket.Cancel();
				// END TASK 6
			}
			// END TASK 3

			// BEGIN TASK 5
			// Check if we hit our take profit
			else if (_takeProfitTicket != null && orderEvent.OrderId == _takeProfitTicket.OrderId) {
				_lastLimitHitAt = Time;

				// BEGIN TASK 6
				// Cancel the stop loss, we no longer need it
				Debug("Cancel:" + _stopLossTicket.OrderId);
				_stopLossTicket.Cancel();
				// END TASK 6
			}
			// END TASK 5
		}
	}
}

//namespace QuantConnect
//{
//	public partial class BootCampTask
//	{
//		public override void OnEndOfAlgorithm()
//		{
//			foreach (var orderTicket in Transactions.GetOrderTickets()) {
//				Debug($"BCT~: Order ticket: {orderTicket.OrderType} {orderTicket.Symbol} @ {orderTicket.Time}");
//			}
//		}
//	}
//}