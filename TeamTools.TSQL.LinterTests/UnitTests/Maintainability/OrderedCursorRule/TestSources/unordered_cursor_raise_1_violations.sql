declare cr cursor global forward_only static for
    select a, b, (select top (1) z from bar order by z desc) as t
    from foo;
