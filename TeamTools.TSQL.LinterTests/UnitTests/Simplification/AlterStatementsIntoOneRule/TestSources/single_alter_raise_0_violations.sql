CREATE TABLE foo.bar
(
    ObjectID        INT            NOT NULL
    , CONSTRAINT PK PRIMARY KEY CLUSTERED (ObjectID ASC)
);
GO

ALTER TABLE foo.bar
ADD CONSTRAINT CK CHECK ((Koef <> (0)));
GO
