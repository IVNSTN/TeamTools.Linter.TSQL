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
            ) as ab
