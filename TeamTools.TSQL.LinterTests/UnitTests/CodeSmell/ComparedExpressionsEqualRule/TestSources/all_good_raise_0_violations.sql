SELECT 1
FROM foo f
join bar b
on f.id = b.id
where group_id <> @group_id
and title like 'start%'
and dt between @start and @end
