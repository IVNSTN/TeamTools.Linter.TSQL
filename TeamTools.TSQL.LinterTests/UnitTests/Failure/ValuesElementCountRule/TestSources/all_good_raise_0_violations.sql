insert t
values ('a', 'b'), ('c', 'd');

select * from
(values ('a', 'b'), ('c', 'd')) v(x, y);
