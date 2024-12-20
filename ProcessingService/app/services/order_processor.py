from datetime import datetime
import asyncio
from app.models.order import Order, OrderStatus
import random

class OrderProcessor:
    def __init__(self):
        self.processing_times = {
            "SMALL": (1, 3),    # 1-3 seconds
            "MEDIUM": (2, 5),   # 2-5 seconds
            "LARGE": (3, 7)     # 3-7 seconds
        }

    async def process_order(self, order: Order) -> Order:
        """
        Process an order with simulated business logic:
        - Validate order
        - Determine processing time based on order complexity
        - Simulate processing with random success/failure
        """
        try:
            #Update status to processing
            order.status = OrderStatus.PROCESSING
            
            # Simulate processing time based on random order "size"
            order_size = random.choice(["SMALL", "MEDIUM", "LARGE"])
            min_time, max_time = self.processing_times[order_size]
            processing_time = random.uniform(min_time, max_time)
            
            # Simulate processing
            await asyncio.sleep(processing_time)
            
            # Simulate 80% success rate
            if random.random() < 0.8:
                order.status = OrderStatus.COMPLETED
            else:
                order.status = OrderStatus.FAILED
                order.error_message = f"Processing failed for {order_size} order after {processing_time:.1f} seconds"
            
            order.processed_at = datetime.utcnow()
            return order
            
        except Exception as e:
            order.status = OrderStatus.FAILED
            order.error_message = str(e)
            order.processed_at = datetime.utcnow()
            return order