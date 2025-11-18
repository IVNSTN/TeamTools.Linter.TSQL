-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    id   INT  NOT NULL IDENTITY(1, 1)
    , a1 INT  NULL
    , b1 INT  NULL
    , c1 INT  NULL
    , d1 INT  NULL
    , e1 INT  NULL
    , f1 INT  NULL
    , g1 INT  NULL
    , h1 INT  NULL
    , i1 INT  NULL
    , j1 INT  NULL
    , k1 INT  NULL
    , l1 INT  NULL
    , m1 INT  NULL
    , n1 INT  NULL
    , o1 INT  NULL
    , p1 INT  NULL
    , q1 INT  NULL
    , r1 INT  NULL
    , s1 INT  NULL
    , t1 INT  NULL
    , u1 INT  NULL
    , v1 INT  NULL
    , w1 INT  NULL
    , x1 INT  NULL
    , y1 INT  NULL
    , z1 INT  NULL
    , a2 INT  NULL
    , b2 INT  NULL
    , c2 INT  NULL
    , d2 INT  NULL
    , e2 INT  NULL
    , f2 INT  NULL
    , g2 INT  NULL
    , h2 INT  NULL
    , i2 INT  NULL
    , j2 INT  NULL
    , k2 INT  NULL
    , l2 INT  NULL
    , m2 INT  NULL
    , n2 INT  NULL
    , o2 INT  NULL
    , p2 INT  NULL
    , q2 INT  NULL
    , r2 INT  NULL
    , s2 INT  NULL
    , t2 INT  NULL
    , u2 INT  NULL
    , v2 INT  NULL
    , w2 INT  NULL
    , x2 INT  NULL
    , y2 INT  NULL
    , z2 INT  NULL
    , a3 INT  NULL
    , b3 INT  NULL
    , c3 INT  NULL
    , d3 INT  NULL
    , e3 INT  NULL
    , f3 INT  NULL
    , g3 INT  NULL
    , h3 INT  NULL
    , i3 INT  NULL
    , j3 INT  NULL
    , k3 INT  NULL
    , l3 INT  NULL
    , m3 INT  NULL
    , n3 INT  NULL
    , o3 INT  NULL
    , p3 INT  NULL
    , q3 INT  NULL
    , r3 INT  NULL
    , s3 INT  NULL
    , t3 INT  NULL
    , u3 INT  NULL
    , v3 INT  NULL
    , w3 INT  NULL
    , x3 INT  NULL
    , y3 INT  NULL
    , z3 INT  NULL
    , a4 INT NULL
    , b4 INT NULL
    , c4 INT NULL
    , d4 INT NULL
    , e4 INT NULL
    , f4 INT NULL
    , g4 INT NULL
    , h4 INT NULL
    , i4 INT NULL
    , j4 INT NULL
    , k4 INT NULL
    , l4 INT NULL
    , m4 INT NULL
    , n4 INT NULL
    , o4 INT NULL
    , p4 INT NULL
    , q4 INT NULL
    , r4 INT NULL
    , s4 INT NULL
    , t4 INT NULL
    , u4 INT NULL
    , v4 INT NULL
    , w4 INT NULL
    , x4 INT NULL
    , y4 INT NULL
    , z4 INT NULL
    , dt DATE NULL
);
GO

CREATE CLUSTERED COLUMNSTORE INDEX ix
    ON dbo.foo
    WITH (DATA_COMPRESSION = COLUMNSTORE_ARCHIVE)
    ON dateranges(dt);
GO
