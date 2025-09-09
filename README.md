# Digitale Delta - CSharp Libraries

The provided libraries are meant to replace the Microsoft OData libraries for C# solutions, tailored for Digitale Delta API's.
They do not offer a full OData V4 functionality, but a useful subset: $filter, $orderby, $count and $select.

The libraries are easy in use, do not rely on specific database architecture of compontents (such as Entity Framework).
The OData filter layer is fully separated from the underlying data model.

The components are:

- DigitaleDelta.Contracts (shared data classes)
- DigitaleDelta.CsdlParser (simple, fast CSDL parser)
- DigitaleDelta.ODataTranslator (fast, low memory usage OData query to SQL translator, build on configuration)
- DigitaleDelta.ODataWriter (fast, low memory usage OData response writer using fluent syntax)
