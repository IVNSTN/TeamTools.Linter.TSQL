-- compatibility level min: 150
MERGE acme a
using foo f
on a.id = f.id
when matched then update set date = f.date
output $action, $node_id
into dbo.log(action, node_id);
