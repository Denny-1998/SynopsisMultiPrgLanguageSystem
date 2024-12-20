from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    API_PORT: int = 8000
    API_HOST: str = "127.0.0.1"
    
    # RabbitMQ Settings
    RABBITMQ_URL: str = "amqp://guest:guest@rabbitmq:5672/"
    ORDER_QUEUE: str = "orders"
    STATUS_QUEUE: str = "order_status"
    
    # Database Settings
    DATABASE_URL: str = "postgresql+asyncpg://postgres:postgres@postgres/orderprocessing"

settings = Settings()