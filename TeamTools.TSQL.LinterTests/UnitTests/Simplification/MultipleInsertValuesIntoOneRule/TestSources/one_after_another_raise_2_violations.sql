insert @t
values (1, 2)

insert @t
values (1, 2, 3)    -- 1

insert foo
values
(null, null),
(null, null)

insert dbo.[foo]
values ('')         -- 2
