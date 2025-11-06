DECLARE @foo TABLE (place_code VARCHAR(20) NOT NULL, place_name VARCHAR(50) NOT NULL, PRIMARY KEY (place_code, place_name));

INSERT INTO @bar (place_code, place_name)
VALUES
('JAR', 'CAR');

DELETE rz
FROM @foo AS pt
INNER JOIN dbo.far AS rt
    ON rt.some_type_id = pt.some_type_id
INNER JOIN @bar AS rz
    ON rz.place_code = pt.place_code
        AND rz.place_name = rt.place_name;
