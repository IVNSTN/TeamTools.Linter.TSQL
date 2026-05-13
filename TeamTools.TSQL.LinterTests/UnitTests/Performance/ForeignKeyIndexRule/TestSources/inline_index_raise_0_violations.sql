-- compatibility level min: 130
CREATE TABLE bar
(
    id          INT,
    category_id INT FOREIGN KEY REFERENCES bar_categories (id),
    some_valued DECIMAL(18,3),
    INDEX ix (category_id, some_valued) -- starting from FK fields is fine
)
GO
