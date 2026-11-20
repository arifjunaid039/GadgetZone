# The Gadget Zone Official — Development Phases

Build the website in the following order. Each phase has a clear deliverable so progress can be checked before moving on.

| Phase | What I will make | Completion checkpoint |
|---|---|---|
| **1. Set up the solution — COMPLETE** | Used the installed .NET 10 SDK to create the solution and the Web, Business, and DataAccess projects. Added the layer references and a root `.gitignore`. | `dotnet build TheGadgetZoneOfficial.slnx` succeeds with zero warnings and zero errors. |
| **2. Design the database — COMPLETE** | Defined the Identity-backed User, Category, Product, CartItem, Order, and OrderItem models with relationships, constraints, and EF Core configurations. | Initial EF Core migration generated successfully. |
| **3. Create database seed data — IMPLEMENTED** | Added repeatable seeding for eight categories and Customer/Admin roles. An initial Admin is provisioned only when local `InitialAdmin` secrets are set. | Startup applies migrations and seeds categories/roles; requires running the app against LocalDB to verify locally. |
| **4. Build data access — IMPLEMENTED** | Added the Business repository contract, EF Core repository, and serializable transaction for order placement. | Data layer and Web project compile; runtime database verification remains subject to local Application Control policy. |
| **5. Build business services — IMPLEMENTED** | Added stock checks, cart updates, server-side totals, checkout validation, product/category validation, and allowed order status transitions. | Business unit tests added for stock, totals, and status rules. |
| **6. Make the website header — IMPLEMENTED** | Built shared responsive navigation for Home, Products, Contact, Cart, Orders, sign-in/out, and role-protected Admin link. | Header is in the shared MVC layout. |
| **7. Make the home and category pages — IMPLEMENTED** | Added a landing page with category navigation and featured product cards. | Home screen uses seeded category/product data. |
| **8. Make product listing, search, and details — IMPLEMENTED** | Added product search, category filtering, stock display, product details, and add-to-cart action. | Product screens compile and use active catalogue records. |
| **9. Make the website footer and Contact Us page — IMPLEMENTED** | Added exact required footer text and Contact Us page with the provided map and coordinates. | Shared footer and location link are present. |
| **10. Make customer registration and login — IMPLEMENTED** | Added Identity registration/login/logout and role assignment; public users receive Customer role only. | Account forms and Identity configuration compile. |
| **11. Make the shopping cart and checkout — IMPLEMENTED** | Added customer-scoped cart actions and COD checkout with current product-price calculations and transaction-based stock reduction. | Business tests cover total calculation; database workflow still needs a local run. |
| **12. Make order history and details — IMPLEMENTED** | Added authenticated order list/detail with customer ownership filtering. | Repository applies user ID filtering for customer order queries. |
| **13. Make the Admin dashboard and management pages — IMPLEMENTED** | Added role-protected dashboard, category/product/stock CRUD, and order/status management. | Controllers enforce the Admin role and validate status transitions. |
| **14. Test, polish, and prepare the assignment — PARTIALLY VERIFIED** | Added setup README and business tests; the solution builds and tests pass. Fresh database startup and browser verification could not run because Windows Application Control blocked loading the Web assembly. | `dotnet test` passes (3/3); start the app on a machine/policy that permits the built Web assembly to complete runtime verification. |

## Recommended order for the layout phases

1. Make the shared MVC layout.
2. Build the header and check its navigation links.
3. Build the footer and confirm it displays on every page.
4. Add the page-specific content between the header and footer.
5. Test the layout at desktop and mobile widths.
