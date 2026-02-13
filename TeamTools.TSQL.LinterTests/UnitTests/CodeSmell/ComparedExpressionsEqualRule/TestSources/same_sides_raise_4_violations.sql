SELECT 1
FROM foo f
join bar b
on f.id = ((f.id))                  -- 1
where group_id <> group_id          -- 2
and title like title                -- 3
and dt between dt and @end          -- 4
