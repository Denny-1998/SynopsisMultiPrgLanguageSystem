import pytest
from fastapi.testclient import TestClient
from httpx import AsyncClient
import pytest_asyncio
from app.main import app
from unittest.mock import AsyncMock, patch
from app.models.order import OrderStatus, Order
from app.models.database import OrderDB
from datetime import datetime

@pytest_asyncio.fixture
async def async_client():
    async with AsyncClient(app=app, base_url="http://test") as client:
        yield client

@pytest.mark.asyncio
@patch('app.main.async_session')
async def test_create_order(mock_session, async_client):
    # Setup mock session
    session = AsyncMock()
    mock_session.return_value.__aenter__.return_value = session
    
    # Mock the get method to return None (order doesn't exist yet)
    session.get = AsyncMock(return_value=None)
    
    # Mock the add method
    session.add = AsyncMock()
    
    # Mock the commit
    session.commit = AsyncMock()

    response = await async_client.post("/orders/test-order-1/process")
    assert response.status_code == 200
    assert "Order test-order-1 received" in response.json()["message"]

    # Verify add was called with an OrderDB instance
    assert session.add.called
    called_with = session.add.call_args[0][0]
    assert isinstance(called_with, OrderDB)
    assert called_with.order_id == "test-order-1"
    assert called_with.status == OrderStatus.RECEIVED

@pytest.mark.asyncio
@patch('app.main.async_session')
async def test_get_nonexistent_order(mock_session, async_client):
    session = AsyncMock()
    session.get = AsyncMock(return_value=None)
    mock_session.return_value.__aenter__.return_value = session
    
    response = await async_client.get("/orders/nonexistent-order")
    assert response.status_code == 404