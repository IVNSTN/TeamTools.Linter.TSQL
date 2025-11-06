GRANT SELECT ON object::foo.bar
TO asdf
AS [dbo];

-- schemas are supposed to be ignored
GRANT SELECT ON schema::foo.bar
TO asdf;

REVOKE SELECT ON object::bar.zar
FROM asdf;

-- types are supposed to be ignored
GRANT EXEC ON type::jar.far
TO asdf;

DENY EXEC ON type::jar.far
TO asdf;

-- services are supposed to be ignored
GRANT SEND ON service::farfar
TO asdf;

DENY SEND ON service::farfar
TO asdf;
