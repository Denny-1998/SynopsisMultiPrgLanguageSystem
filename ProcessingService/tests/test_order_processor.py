import pytest
from app.services.order_processor import OrderProcessor
from app.models.order import Order, OrderStatus
from datetime import datetime

@pytest.fixture
def order_processor():
    return OrderProcessor()

@pytest.fixture
def sample_order():
    return Order(
        order_id="test-123",
        status=OrderStatus.RECEIVED,
        created_at=datetime.utcnow()
    )

@pytest.mark.asyncio
async def test_process_order_changes_status(order_processor, sample_order):
    # Process the order
    processed_order = await order_processor.process_order(sample_order)
    
    # Assert status changed from RECEIVED
    assert processed_order.status != OrderStatus.RECEIVED
    assert processed_order.processed_at is not None

@pytest.mark.asyncio
async def test_process_order_sets_error_message_when_failed(order_processor, sample_order):
    # Process orders until we get a failed one
    processed_order = None
    for _ in range(10):
        processed_order = await order_processor.process_order(sample_order)
        if processed_order.status == OrderStatus.FAILED:
            break
    
    if processed_order.status == OrderStatus.FAILED:
        assert processed_order.error_message is not None