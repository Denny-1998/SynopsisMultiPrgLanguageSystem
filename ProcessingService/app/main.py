from fastapi import FastAPI, HTTPException, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from datetime import datetime
from typing import List
from sqlalchemy import select
from sqlalchemy.ext.asyncio import create_async_engine, AsyncSession
from sqlalchemy.orm import sessionmaker

from app.config import settings
from app.models.order import Order, OrderStatus
from app.models.database import Base, OrderDB
from app.services.order_processor import OrderProcessor
from app.services.message_broker import MessageBroker


print("Starting API")
app = FastAPI(title="Order Processing Service")
order_processor = OrderProcessor()
message_broker = MessageBroker()

# Create async engine
engine = create_async_engine(
    "postgresql+asyncpg://postgres:postgres@postgres/orderprocessing",
    echo=True
)

# Create the async sessionmaker
async_session = sessionmaker(
    bind=engine,
    class_=AsyncSession,
    expire_on_commit=False
)

@app.on_event("startup")
async def startup():
    # Create tables
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
    # Connect to RabbitMQ
    await message_broker.connect()

@app.on_event("shutdown")
async def shutdown():
    await message_broker.close()

async def process_order_background(order_id: str):
    async with async_session() as session:
        # Get order from database
        order_db = await session.get(OrderDB, order_id)
        if not order_db:
            return
            
        # Convert to Order model
        order = Order(
            order_id=order_db.order_id,
            status=order_db.status,
            created_at=order_db.created_at,
            processed_at=order_db.processed_at,
            error_message=order_db.error_message
        )
        
        # Process order
        processed_order = await order_processor.process_order(order)
        
        # Update database
        order_db.status = processed_order.status
        order_db.processed_at = processed_order.processed_at
        order_db.error_message = processed_order.error_message
        await session.commit()
        
        # Publish status update
        await message_broker.publish_status(processed_order)






@app.post("/orders/{order_id}/process")
async def process_order(order_id: str, background_tasks: BackgroundTasks):
    async with async_session() as session:
        # Create new order in db
        order_db = OrderDB(
            order_id=order_id,
            status=OrderStatus.RECEIVED,
            created_at=datetime.utcnow()
        )
        session.add(order_db)
        await session.commit()

        # Start background processing
        background_tasks.add_task(process_order_background, order_id)
    return {"message": f"Order {order_id} received and processing started"}


@app.get("/orders/{order_id}")
async def get_order(order_id: str):
    async with async_session() as session:
        order_db = await session.get(OrderDB, order_id)
        if not order_db:
            raise HTTPException(status_code=404, detail="Order not found")
        
        return Order(
            order_id=order_db.order_id,
            status=order_db.status,
            created_at=order_db.created_at,
            processed_at=order_db.processed_at,
            error_message=order_db.error_message
        )

@app.get("/orders")
async def list_orders() -> List[Order]:
    async with async_session() as session:
       
        result = await session.execute(select(OrderDB))
        orders_db = result.scalars().all()
        
        return [
            Order(
                order_id=order.order_id,
                status=order.status,
                created_at=order.created_at,
                processed_at=order.processed_at,
                error_message=order.error_message
            ) for order in orders_db
        ]




if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8090)