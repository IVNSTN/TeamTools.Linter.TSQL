SELECT TOP 1
       1
FROM dbo.dh
WHERE dh.act_id = @act_id
    AND
    ((dh.last_buy_date >= @run_date AND dh.state_id = 2)
        OR (dh.last_buy_date < @run_date AND dh.pay_date IS NOT NULL AND dh.pay_date < @run_date AND dh.state_id = 1)
        OR (dh.last_buy_date < @run_date AND dh.pay_date IS NOT NULL AND dh.pay_date >= @run_date AND dh.state_id = 2)
    );
GO

SELECT 1
WHERE pos.IdSubAccountMod = @SectNum
    AND
    (
        (
            mbbst.IdSession = pos.BackIdSession
            AND ISNULL(pos.SumTrnIn, 0) = 0
            AND ISNULL(pos.SumTrnOut, 0) = 0
            AND ISNULL(pos.SumBuy, 0) = 0
            AND ISNULL(pos.SumSell, 0) = 0
            AND ISNULL(pos.SumBuyVal, 0) = 0
            AND ISNULL(pos.SumSellVal, 0) = 0
            AND ISNULL(pos.VM, 0) = 0
            AND ISNULL(pos.Pos, 0) = 0
            AND ISNULL(pos.NalPos, 0) = 0
            AND ISNULL(pos.NalSaldo, 0) = 0
            AND ISNULL(pos.BackPos, 0) = 0
            AND ISNULL(pos.BackNalPos, 0) = 0
            AND ISNULL(pos.BackdbD0Qty, 0) = 0
            AND ISNULL(pos.BackdbD1Qty, 0) = 0
            AND ISNULL(pos.BackdbD2Qty, 0) = 0
            AND ISNULL(pos.BackdbD3Qty, 0) = 0
            AND ISNULL(pos.BackdbD4Qty, 0) = 0
            AND ISNULL(pos.BackdbD5Qty, 0) = 0
            AND ISNULL(pos.BackdbD6Qty, 0) = 0
            AND ISNULL(pos.BackdbD7Qty, 0) = 0
            AND ISNULL(pos.BackdbD8Qty, 0) = 0
            AND ISNULL(pos.BackdbD9Qty, 0) = 0
            AND ISNULL(pos.BackdbD10Qty, 0) = 0
            AND ISNULL(pos.BackdbDplusQty, 0) = 0
        )
        OR
        ((mbbst.IdSession - 2) > pos.BackIdSession
            AND mbbst.Date > pos.LastDateSession
            AND ISNULL(pos.SumTrnIn, 0) = 0
            AND ISNULL(pos.SumTrnOut, 0) = 0
            AND ISNULL(pos.SumBuy, 0) = 0
            AND ISNULL(pos.SumSell, 0) = 0
            AND ISNULL(pos.SumBuyVal, 0) = 0
            AND ISNULL(pos.SumSellVal, 0) = 0
            AND ISNULL(pos.VM, 0) = 0
            AND ISNULL(pos.Pos, 0) = 0
            AND ISNULL(pos.NalPos, 0) = 0
            AND ISNULL(pos.NalSaldo, 0) = 0
            AND ISNULL(pos.BackPos, 0) = 0
            AND ISNULL(pos.BackNalPos, 0) = 0
            AND ISNULL(pos.BackdbD0Qty, 0) = 0
            AND ISNULL(pos.BackdbD1Qty, 0) = 0
            AND ISNULL(pos.BackdbD2Qty, 0) = 0
            AND ISNULL(pos.BackdbD3Qty, 0) = 0
            AND ISNULL(pos.BackdbD4Qty, 0) = 0
            AND ISNULL(pos.BackdbD5Qty, 0) = 0
            AND ISNULL(pos.BackdbD6Qty, 0) = 0
            AND ISNULL(pos.BackdbD7Qty, 0) = 0
            AND ISNULL(pos.BackdbD8Qty, 0) = 0
            AND ISNULL(pos.BackdbD9Qty, 0) = 0
            AND ISNULL(pos.BackdbD10Qty, 0) = 0
            AND ISNULL(pos.BackdbDplusQty, 0) = 0
        )
    )
