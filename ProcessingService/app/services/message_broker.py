import aio_pika
from app.config import settings
import json
from app.models.order import Order

class MessageBroker:
    def __init__(self):
        self.connection = None
        self.channel = None

    async def connect(self):
        self.connection = await aio_pika.connect_robust(settings.RABBITMQ_URL)
        self.channel = await self.connection.channel()
        
        # Declare queues
        await self.channel.declare_queue(settings.ORDER_QUEUE)
        await self.channel.declare_queue(settings.STATUS_QUEUE)

    async def publish_status(self, order: Order):
        if not self.channel:
            await self.connect()
            
        message = aio_pika.Message(
            json.dumps({
                "order_id": order.order_id,
                "status": order.status,
                "processed_at": order.processed_at.isoformat() if order.processed_at else None,
                "error_message": order.error_message
            }).encode()
        )
        
        await self.channel.default_exchange.publish(
            message,
            routing_key=settings.STATUS_QUEUE
        )

    async def close(self):
        if self.connection:
            await self.connection.close()