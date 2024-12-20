from pydantic import BaseModel
from typing import Optional
from enum import Enum
from datetime import datetime

class OrderStatus(str, Enum):
    RECEIVED = "received"
    PROCESSING = "processing"
    COMPLETED = "completed"
    FAILED = "failed"

class Order(BaseModel):
    order_id: str
    status: OrderStatus
    created_at: datetime
    processed_at: Optional[datetime] = None
    error_message: Optional[str] = None