select '495 adsf 926 --- 00011' as phone,
    t.phone
from pnohes as t

select '9999999999', 11223344, '899999999990' -- magic number ignored
select '2010-01-09', '2010/02/09' -- date ignored
select '123-321098-09809312-21431' -- magic number ignored
select '11902828-0011-4015-1451-118822275042' -- guid ignored

select '123
                    1' -- many spaces
