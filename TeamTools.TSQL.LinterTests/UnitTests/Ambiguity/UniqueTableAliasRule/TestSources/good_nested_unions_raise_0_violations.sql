            select *
            from
            (
select * from tbl_a as a
inner join tbl_b as b
    on a_id = b_id
union all
select * from src_a as a
inner join src_b as b
    on a_id = b_id
union
select * from src_a as a
inner join src_b as b
    on a_id = b_id
where not exists(
    select * from tbl_a as aa
    inner join tbl_b as bb
        on a_id = b_id
    intersect
    select * from src_a as aa
    inner join src_b as bb
        on a_id = b_id
    except
    select * from src_a as aa
    inner join src_b as bb
        on a_id = b_id
    )
            ) as ab
