DECLARE @sub_acc_id_list mrgn.sub_acc_id_list

IF ISNULL(@is_update_required, 1) = 1
    -- select from is not a "variable reference" we expect
    -- there is a separate rule about "always empty source"
    AND NOT EXISTS (SELECT 1 FROM @sub_acc_id_list WHERE sub_acc_id = @sub_acc_id)
BEGIN
    INSERT INTO @sub_acc_id_list (sub_acc_id) VALUES (@sub_acc_id);
END;
