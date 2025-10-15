# E-Commerce Backend Setup

## Environment Variables

Create a `.env` file in the backend root directory with the following variables:

```env
# MongoDB Configuration
MONGODB_CONNECTION_STRING=mongodb://localhost:27017
MONGODB_DATABASE_NAME=ecommerce_db

# Supabase Configuration (for JWT validation)
SUPABASE_PROJECT_REF=your_supabase_project_ref

# CORS Configuration
ALLOWED_ORIGINS=http://localhost:3000,https://your-frontend-domain.com

# Optional: Supabase for file storage
SUPABASE_URL=your_supabase_project_url
SUPABASE_KEY=your_supabase_service_role_key
```

## Installation

1. Install dependencies:
```bash
dotnet restore
```

2. Run the application:
```bash
dotnet run
```

The API will be available at `http://localhost:5000`

## API Endpoints

### Products (Public)
- `GET /api/products` - Get all products (paginated)
- `GET /api/products/{id}` - Get product by ID

### Products (Authenticated)
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Cart (Authenticated)
- `GET /api/cart` - Get user's cart
- `POST /api/cart/items` - Add item to cart
- `PATCH /api/cart/items/{productId}` - Update cart item quantity
- `DELETE /api/cart/items/{productId}` - Remove item from cart
- `DELETE /api/cart` - Clear cart

### Orders (Authenticated)
- `POST /api/orders` - Create order from cart
- `GET /api/orders/my` - Get user's orders
- `GET /api/orders/{id}` - Get order by ID
- `PATCH /api/orders/{id}/status` - Update order status

## Authentication

The API uses Supabase JWT tokens for authentication. Include the token in the Authorization header:

```
Authorization: Bearer <supabase_jwt_token>
```

## Database

The application uses MongoDB for data persistence. Collections:
- `Products` - Product information
- `Carts` - User shopping carts
- `Orders` - User orders

