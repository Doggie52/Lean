using System;

using QuantConnect.Data;
using QuantConnect.Orders;

namespace QuantConnect.Algorithm.CSharp.Bootcamp1
{
	class OrderManagementBootCampLesson : QCAlgorithm
	{
		// Main asset we intend to trade
		private string _mainAssetTicker = "SPY";

		// Stop loss price as a percentage of main asset close price
		private decimal _stopLossRatio = 0.90m;

		// Order ticket for our stop loss
		private OrderTicket _stopLossTicket;

		// BEGIN TASK 3
		// Datetime when stop loss or take profit was last hit
		private DateTime _lastLimitHitAt;
		// END TASK 3

		// BEGIN TASK 4
		// The highest close price our main asset has achieved since placing our market order
		private decimal _highestClose = -1m;
		// END TASK 4

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
			// BEGIN TASK 7
			// Plot the asset price in a separate chart
			Plot( "Data chart", "Asset price", Securities[_mainAssetTicker].Close );
			// END TASK 7

			// BEGIN TASK 3
			// Check that at least 10 days (~2 weeks) have passed since we last hit our limit order
			if ( ( Time - _lastLimitHitAt ).TotalDays < 10 )
				return;
			// END TASK 3

			if ( !Portfolio.Invested ) {

				// Create market order for some units of SPY
				MarketOrder( _mainAssetTicker, 500 );

				// Create stop loss through a stop market order
				_stopLossTicket = StopMarketOrder( _mainAssetTicker, -500, _stopLossRatio * Securities[_mainAssetTicker].Close );

				// BEGIN TASK 4
				// Store current price as the highest price
				_highestClose = Securities[_mainAssetTicker].Close;
				// END TASK 4

				// BEGIN TASK 5
				// Create take profit through a limit order
				_takeProfitTicket = LimitOrder( _mainAssetTicker, -500, _takeProfitRatio * Securities[_mainAssetTicker].Close );
				// END TASK 5

			} else {

				// BEGIN TASK 4
				// Update stop loss price if main asset has risen above its highest price
				if ( Securities[_mainAssetTicker].Close >= _highestClose ) {
					_stopLossTicket.Update( new UpdateOrderFields() { StopPrice = Securities[_mainAssetTicker].Close * _stopLossRatio } );

					_highestClose = Securities[_mainAssetTicker].Close;
				}
				// END TASK 4

				// BEGIN TASK 7
				// Plot the current stop loss price
				Plot( "Data chart", "Stop loss price", _stopLossTicket.Get( OrderField.StopPrice ) );

				// Plot the current take profit price
				Plot( "Data chart", "Take profit price", _takeProfitTicket.Get( OrderField.LimitPrice ) );
				// END TASK 7
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
			// END TASK 2

			// BEGIN TASK 3
			// Check if we hit our stop loss
			if ( _stopLossTicket != null && orderEvent.OrderId == _stopLossTicket.OrderId ) {
				_lastLimitHitAt = Time;

				// BEGIN TASK 6
				// Cancel the take profit, we no longer need it
				_takeProfitTicket.Cancel();
				// END TASK 6
			}
			// END TASK 3

			// BEGIN TASK 5
			// Check if we hit our take profit
			else if ( _takeProfitTicket != null && orderEvent.OrderId == _takeProfitTicket.OrderId ) {
				_lastLimitHitAt = Time;

				// BEGIN TASK 6
				// Cancel the stop loss, we no longer need it
				_stopLossTicket.Cancel();
				// END TASK 6
			}
			// END TASK 5
		}
	}
}