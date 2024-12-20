from sqlalchemy import Column, String, DateTime, Enum as SQLAEnum
from sqlalchemy.ext.declarative import declarative_base
from datetime import datetime
from app.models.order import OrderStatus

Base = declarative_base()

class OrderDB(Base):
    __tablename__ = "orders"
    
    order_id = Column(String, primary_key=True)
    status = Column(SQLAEnum(OrderStatus))
    created_at = Column(DateTime, default=datetime.utcnow)
    processed_at = Column(DateTime, nullable=True)
    error_message = Column(String, nullable=True)