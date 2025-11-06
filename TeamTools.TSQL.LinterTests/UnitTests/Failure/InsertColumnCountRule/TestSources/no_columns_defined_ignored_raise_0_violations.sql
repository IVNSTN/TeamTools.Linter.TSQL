insert into bar
values ('asf', 1, NULL);

update x set a = b
    output inserted.a, deleted.b
    into far
from xxx as x
