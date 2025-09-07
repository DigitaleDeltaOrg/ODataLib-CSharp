# Security

## ODataWriter Security Considerations

The ODataWriter creates dynamic WHERE clauses based on the OData filter expressions and maps for functions and properties.

It is crucial to ensure that the generated SQL does not expose the application to SQL injection attacks or other security vulnerabilities.

The library creates SQL parameterised queries to mitigate the risk of SQL injection.

While parameterized queries provide robust protection against SQL injection:

1. The library never uses string concatenation for query values
2. All user-provided filter values are properly parameterised
3. Column and table identifiers are controlled through property maps, not user input
4. Dynamic query structures are validated before execution

Users of this library should ensure they only use trusted property maps and function maps.

## Abstraction as a Security Layer

The library provides an additional security benefit through abstraction:

1. End users can only issue OData requests against a defined OData model
2. The OData model is intentionally distinct from the underlying database schema
3. This abstraction layer prevents direct access to database structures
4. Only the explicitly mapped properties and functions are exposed to queries
5. SQL comment injection attacks (using `--` or `/* */`) are prevented since:
   - Comments in parameterized values are treated as literal data
   - Property maps prevent comment injection in identifiers
   - The abstraction layer makes it difficult to craft effective comment-based attacks

This "security by design" approach limits the attack surface by ensuring that even
if a malicious query were constructed, it could only target the explicitly mapped elements
rather than the actual database schema.

This library makes it also much easier to separate the OData definition from the database model than the Microsoft OData libraries.
