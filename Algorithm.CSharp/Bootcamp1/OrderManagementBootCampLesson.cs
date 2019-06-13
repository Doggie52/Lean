using System;

using QuantConnect.Data;
using QuantConnect.Orders;

namespace QuantConnect.Algorithm.CSharp.Bootcamp1
{
	class OrderManagementBootCampLesson : QCAlgorithm
	{

		// BEGIN TASK 2
		private DateTime _lastLimitHitAt;
		// END TASK 2

		// Main asset we intend to trade
		private string _mainAssetTicker = "SPY";

		// Stop loss price as a percentage of main asset close price
		private decimal _stopLossRatio = 0.90m;

		// Order ticket for our stop loss
		private OrderTicket _stopLossTicket;

		// BEGIN TASK 5
		// Take profit price as a percentage of main asset close price
		private decimal _takeProfitRatio = 1.10m;

		// Order ticket for our take profit
		private OrderTicket _takeProfitTicket;
		// END TASK 5

		public override void Initialize()
		{
			SetStartDate( 2018, 12, 1 );
			SetEndDate( 2019, 4, 1 );
			SetCash( 100000 );

			AddSecurity( SecurityType.Equity, _mainAssetTicker, Resolution.Daily );
		}

		public override void OnData( Slice slice )
		{

			// BEGIN TASK 4
			// Plot the asset price in a separate chart
			Plot( "Data chart", "Asset price", Securities[_mainAssetTicker].Close );
			// END TASK 4

			// BEGIN TASK 2
			// Check that at least 10 days (~2 weeks) have passed since we last hit our limit order
			if ( ( Time - _lastLimitHitAt ).TotalDays < 10 )
				return;
			// END TASK 2

			if ( !Portfolio.Invested ) {
				// We are not yet invested

				// Create market order for some units of SPY
				MarketOrder( Securities[_mainAssetTicker].Symbol, 500 );

				// Create stop loss through a stop market order
				_stopLossTicket = StopMarketOrder( Securities[_mainAssetTicker].Symbol, -500, _stopLossRatio * Securities[_mainAssetTicker].Close );

				// BEGIN TASK 5
				// Create take profit through a limit order
				_takeProfitTicket = LimitOrder( Securities[_mainAssetTicker].Symbol, -500, _takeProfitRatio * Securities[_mainAssetTicker].Close );
				// END TASK 5

			} else {
				// We are already invested

				// BEGIN TASK 3
				// Update stop loss price if main asset has risen by 1% or more since we last updated the stop loss
				if ( Securities[_mainAssetTicker].Close >= 1.01m * ( _stopLossTicket.Get( OrderField.StopPrice ) / _stopLossRatio ) ) {
					_stopLossTicket.Update( new UpdateOrderFields() { StopPrice = Securities[_mainAssetTicker].Close * _stopLossRatio } );
				}
				// END TASK 3

				// BEGIN TASK 4
				// Plot the current stop loss price
				Plot( "Data chart", "Stop loss price", _stopLossTicket.Get( OrderField.StopPrice ) );
				// END TASK 4

				// BEGIN TASK 5
				// Plot the current take profit price
				Plot( "Data chart", "Take profit price", _takeProfitTicket.Get( OrderField.LimitPrice ) );
				// END TASK 5
			}
		}

		public override void OnOrderEvent( OrderEvent orderEvent )
		{
			// BEGIN TASK 2
			// Only act on fills (ignore submits)
			if ( orderEvent.Status != OrderStatus.Filled )
				return;

			// Log order fill price (can be extended to log more information)
			Log( $"{orderEvent.FillPrice}" );

			// Check if we hit our stop loss
			// @todo clean
			if ( _stopLossTicket != null && orderEvent.OrderId == _stopLossTicket.OrderId ) {
				_lastLimitHitAt = Time;

				// BEGIN TASK 5
				// Cancel the take profit, we no longer need it
				_takeProfitTicket.Cancel();
				// END TASK 5
			}
			// END TASK 2

			// BEGIN TASK 5
			// Check if we hit our take profit
			// @todo clean
			else if ( _takeProfitTicket != null && orderEvent.OrderId == _takeProfitTicket.OrderId ) {
				_lastLimitHitAt = Time;

				// Cancel the stop loss, we no longer need it
				_stopLossTicket.Cancel();
			}
			// END TASK 5
		}
	}
}