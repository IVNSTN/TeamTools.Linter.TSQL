SELECT fld.dest_treaty.value('(text())[1]', 'INT')
FROM dbo.free_orders AS fr
CROSS APPLY fr.as_xml.nodes('/Document/dest_treaty') AS fld(dest_treaty)
WHERE fr.treaty = @treaty
