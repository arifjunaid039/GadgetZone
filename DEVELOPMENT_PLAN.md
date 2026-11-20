# The Gadget Zone Official — Project Dossier and Development Plan

## 1. Project overview

Build a beginner-friendly online store for tech accessories using ASP.NET Core MVC and a three-layer architecture. Customers can browse products, manage a cart, place Cash on Delivery (COD) orders, and review their order history. Administrators can manage the catalogue, stock, and orders.

**Business:** The Gadget Zone Official, Scheme 33, Karachi  
**Map:** [Open The Gadget Zone Official in Google Maps](https://www.google.com/maps/place/The+Gadget+Zone+Official/@24.9689025,67.1250409,17z/data=!4m7!3m6!1s0x3eb33963e6277c9d:0xff4fb83913e39b57!4b1!8m2!3d24.9698265!4d67.1255189!16s%2Fg%2F11vj_wgj68)  
**Coordinates:** 24.9698265, 67.1255189

## 2. Required tools and technologies

| Purpose | Recommended choice |
|---|---|
| IDE | Visual Studio with the ASP.NET and web development workload |
| Runtime | ASP.NET Core MVC on a supported .NET LTS version; use the version required by the course if specified |
| Language | C# |
| Database | SQL Server Express or LocalDB for local development |
| ORM | Entity Framework Core and its SQL Server provider |
| Authentication | ASP.NET Core Identity with roles (Customer and Admin) |
| UI | Razor views, HTML, CSS, Bootstrap, and JavaScript for small enhancements |
| Source control | Git (optional but recommended) |
| Testing | xUnit for business/service tests; manual browser checks for the MVC workflow |

Install the matching .NET SDK and EF Core tools. Use Visual Studio's Package Manager Console or the `dotnet ef` CLI for migrations. Keep connection strings in local development configuration or user secrets; do not commit real credentials.

## 3. Solution and folder structure

Create one solution with three projects. Keep dependencies flowing from the web and data implementation toward business contracts, not from Business back to the MVC application.

```text
TheGadgetZoneOfficial/
├── TheGadgetZoneOfficial.slnx
├── src/
│   ├── TheGadgetZone.Web/                   # Presentation layer (ASP.NET Core MVC)
│   │   ├── Areas/
│   │   │   └── Admin/
│   │   │       ├── Controllers/
│   │   │       └── Views/
│   │   ├── Controllers/                     # Customer-facing controllers
│   │   ├── Views/                           # Razor pages and shared layout
│   │   ├── ViewModels/                      # Input/display models; never bind entities directly
│   │   ├── wwwroot/                         # CSS, JavaScript, and product images
│   │   ├── Program.cs
│   │   └── appsettings.json                 # No production secrets
│   ├── TheGadgetZone.Business/              # Business layer and domain contracts
│   │   ├── Entities/                        # Category, Product, CartItem, Order, OrderItem
│   │   ├── Interfaces/                      # Repository contracts
│   │   ├── Services/                        # Product, cart, and order rules
│   │   └── DTOs/                            # Data passed between layers
│   └── TheGadgetZone.DataAccess/             # Data access layer
│       ├── Context/                         # EF Core DbContext and configuration
│       ├── Identity/                        # Identity-backed ApplicationUser and roles
│       ├── Repositories/                    # Repository implementations
│       └── Migrations/
└── tests/
    └── TheGadgetZone.Business.Tests/         # Unit tests for business rules
```

**Project references**

- `TheGadgetZone.Web` references `TheGadgetZone.Business` and `TheGadgetZone.DataAccess` (to register implementations at startup).
- `TheGadgetZone.DataAccess` references `TheGadgetZone.Business` (to implement repository contracts and use domain entities).
- `TheGadgetZone.Business` does not reference Web or DataAccess.
- Register services and repositories through dependency injection in `Program.cs`.
- Keep the ASP.NET Core Identity user type in DataAccess so the independent Business layer does not depend on Identity packages.

For a small practical assignment, use focused repository interfaces and service classes rather than adding a generic repository or extra abstraction for every entity.

## 4. Database design

Use SQL Server with EF Core migrations. Use integer primary keys for a straightforward assignment, UTC timestamps where applicable, and `decimal(18,2)` for prices and totals. ASP.NET Core Identity adds its own user, role, and join tables; customize its user type with the fields below rather than storing plain-text passwords.

### Entities and relationships

| Entity | Important fields | Relationships / notes |
|---|---|---|
| **User** (Identity user) | Id, FullName, Email, PhoneNumber, Address fields as needed | One user has many CartItems and Orders. Identity stores password hashes and roles. |
| **Category** | Id, Name, Description, IsActive | One category has many Products. Do not hard-delete a category that still has products. |
| **Product** | Id, CategoryId, Name, Description, Price, StockQuantity, ImageUrl, IsActive, CreatedAt | Each product belongs to one category. Price must be non-negative; stock must be zero or more. |
| **CartItem** | Id, UserId, ProductId, Quantity, AddedAt | Many cart items belong to one user; each references one product. Enforce one row per user/product pair or combine quantities when adding. |
| **Order** | Id, UserId, OrderDate, Status, PaymentMethod, Subtotal, DeliveryAddress, PhoneNumber | One order belongs to one user and has one or more OrderItems. PaymentMethod is COD for this project. |
| **OrderItem** | Id, OrderId, ProductId (nullable if products may be removed), ProductName, UnitPrice, Quantity, LineTotal | Each item belongs to one order. Store product-name and price snapshots so order history remains accurate after catalogue edits. |

**Relationship summary:** `User 1—many CartItems`; `User 1—many Orders`; `Category 1—many Products`; `Product 1—many CartItems`; `Order 1—many OrderItems`; `Product 1—many OrderItems` (optional/null if products can be deleted).

Add foreign keys, sensible required-field and length constraints, indexes for commonly queried fields (such as category, product name, user ID, and order date), and a unique index for category names and user/product cart pairs where appropriate. Prefer deactivating products/categories over deleting records referenced by orders.

### Category seed data

Seed these categories once, using a migration or a safe, repeatable startup seed:

1. Mobile Accessories
2. Earbuds
3. Smartwatches
4. Chargers and Cables
5. Keyboards and Mouse
6. Laptop Bags
7. Gaming Accessories
8. Smart Gadgets

Seed an initial Admin role and administrator account through a controlled setup process. Do not hard-code a real password or grant Admin role through public registration.

## 5. Pages and main routes

### Customer experience

- **Home** — store name, featured products/categories, and links to browse.
- **Product listing** — category filter, search, product cards, price, and availability.
- **Product details** — description, image, price, stock status, and add-to-cart action.
- **Register / Login / Logout** — Identity-powered customer access.
- **Shopping cart** — item quantities, remove action, subtotal, and continue-shopping link.
- **Checkout** — delivery details, order summary, and COD confirmation.
- **Order confirmation** — order number, summary, and status.
- **My orders** — signed-in customer's order history.
- **Order details** — items, totals, delivery details, and current status.
- **Contact Us** — business address and an embedded map or map link using the provided coordinates.
- **Access denied / Not found** — understandable responses for unauthorized or missing content.

### Admin experience

Use an `/Admin` area protected by the Admin role.

- **Admin dashboard** — simple counts for products, categories, pending orders, and low/out-of-stock items.
- **Category management** — list, add, edit, activate/deactivate.
- **Product management** — list, add, edit, activate/deactivate, assign category, and update stock.
- **Order management** — list/filter orders, inspect details, and update status only through permitted transitions.
- **Admin login** — use the same Identity login page with role-based access, or provide a clearly linked admin entry point.

### Shared layout

Provide navigation that changes appropriately for signed-in customers and admins. The shared footer must show exactly: **“The Gadget Zone Official, Scheme 33, Karachi”**.

## 6. Business rules

1. **Authentication and authorization**
   - Passwords are handled by ASP.NET Core Identity; never store or display plain-text passwords.
   - Customers can access only their own cart and orders.
   - Only users in the Admin role can access admin routes and actions.
   - Public registration always creates a Customer, never an Admin.

2. **Products and stock**
   - A product must have a name, valid category, non-negative price, and non-negative stock.
   - Inactive products are hidden from normal browsing and cannot be added to a cart or ordered.
   - An add-to-cart quantity must be at least one and cannot exceed available stock.
   - Recheck current product activity and stock at checkout; cart contents can become stale.

3. **Cart and totals**
   - Cart data is scoped to the logged-in user.
   - Adding the same product again updates its quantity instead of creating duplicates.
   - Quantity changes are validated against current stock; zero or negative quantities are rejected (use a remove action instead).
   - Calculate line totals and subtotal on the server using decimal arithmetic; never trust totals submitted by the browser.
   - For this assignment, do not add tax or shipping charges unless explicitly required; show the calculation clearly.

4. **Checkout and order placement**
   - Require a signed-in customer and valid delivery address/contact information.
   - Support Cash on Delivery only; no online payment processing is in scope.
   - In one database transaction, revalidate stock, create the order and item snapshots, reduce stock, and clear the cart. If any step fails, roll back the transaction and show a useful error.
   - Recalculate totals from database prices at checkout, not from cart or form values.

5. **Order status**
   - Use a small, explicit status set such as `Pending`, `Confirmed`, `Shipped`, `Delivered`, and `Cancelled`.
   - Only admins can change order status; validate allowed transitions (for example, do not move a Delivered order back to Pending).
   - If cancellation is supported, define the cutoff (for example, only Pending/Confirmed orders) and restore stock exactly once. Make the rule visible to the user.
   - Customers may view their own orders but cannot edit order contents or status after placement.

6. **Validation and safety**
   - Validate on both client and server; server-side validation is authoritative.
   - Use anti-forgery validation for state-changing MVC form posts.
   - Use view models and allowlisted fields to avoid over-posting.
   - Encode user-provided text in views; validate image URLs or use a controlled upload strategy if image upload is added.

## 7. Development phases (recommended order)

The detailed, ordered implementation roadmap is in the separate [Development Phases](DEVELOPMENT_PHASES.md) document. It includes individual phases for building the website header and footer.

## 8. Testing checklist

### Build, configuration, and database

- [ ] All three projects build and project references follow the intended direction.
- [ ] App connects to local SQL Server/LocalDB using local configuration, not committed production secrets.
- [ ] A new database can be created by applying migrations.
- [ ] Seed data produces the eight expected categories and a usable Admin role/account.
- [ ] Required fields, decimal precision, foreign keys, and unique constraints are present.

### Authentication and permissions

- [ ] Customer registration and login work; invalid credentials show a useful message.
- [ ] Passwords are not stored as plain text.
- [ ] Public registration cannot create an Admin.
- [ ] Anonymous visitors cannot use cart/checkout/order pages that require login.
- [ ] A customer cannot view another customer's cart or orders.
- [ ] A customer receives Access Denied for admin pages/actions; an Admin can access them.

### Catalogue and admin management

- [ ] All seeded categories display correctly.
- [ ] Category filtering and product search return correct results, including no-result cases.
- [ ] Product details show correct category, price, image/placeholder, and stock state.
- [ ] Invalid product values (negative price/stock, missing name/category) are rejected server-side.
- [ ] Admin can add/edit/deactivate categories and products; inactive products are not purchasable.
- [ ] Stock changes made by Admin are reflected in subsequent browsing and checkout.

### Cart and checkout

- [ ] Adding a product creates a cart item; adding it again updates its quantity.
- [ ] Cart belongs to the correct user and quantities can be changed or removed.
- [ ] Zero/negative quantities and quantities above stock are rejected.
- [ ] Cart totals and checkout totals are calculated server-side and use the current product price.
- [ ] Checkout requires valid contact/delivery information and uses COD.
- [ ] Successful checkout creates the correct order and item price/name snapshots, decrements stock, and clears the cart.
- [ ] Failed checkout (including insufficient stock) does not leave a partial order, incorrectly changed stock, or cleared cart.
- [ ] Two orders competing for the last unit cannot both oversell stock (verify transaction/concurrency strategy as assignment scope allows).

### Orders, location, and usability

- [ ] Order history and detail show the correct items, totals, date, and status.
- [ ] Admin can update order status only through allowed transitions.
- [ ] Cancellation, if implemented, is limited by the stated rule and restores stock at most once.
- [ ] Contact page map/link opens the specified location at 24.9698265, 67.1255189.
- [ ] Footer displays “The Gadget Zone Official, Scheme 33, Karachi”.
- [ ] Forms show validation feedback; state-changing forms reject missing/invalid anti-forgery tokens.
- [ ] Layout and navigation remain understandable on narrow screens and pages have useful empty/error states.

### Automated tests

- [ ] Unit tests cover product validation, stock boundaries, cart merging, total calculations, and order-status transitions.
- [ ] Integration/manual tests cover EF persistence, authentication/authorization, checkout transaction, and customer order ownership.

## 9. Assignment scope notes

Keep the first version focused: one store, one currency (PKR, if required by the instructor), COD only, simple product images, and a single admin role. Defer payment gateways, coupons, reviews, delivery integrations, and multi-vendor features unless the assignment explicitly adds them. Present the project as an academic implementation; confirm production security, privacy, and operational requirements separately before real-world use.
