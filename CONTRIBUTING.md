## Validation Guidelines

All FluentValidation validators MUST include database-level checks:

- ✅ Foreign key existence validation
- ✅ Unique constraint validation  
- ✅ Business rule validation requiring DB queries

Pattern:
- Inject `AppDbContext` into validator constructor
- Use `MustAsync()` for async database checks
- Provide clear, actionable error messages

This prevents 500 errors and provides better user feedback.
