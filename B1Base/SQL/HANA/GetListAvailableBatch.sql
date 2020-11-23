SELECT  "OBTQ"."SysNumber",
		"OBTN"."DistNumber",
		(COALESCE("OBTQ"."Quantity",0) - COALESCE("OBTQ"."CommitQty",0)) AS "Quantity"
FROM "OBTQ"
INNER JOIN "OBTN" ON "OBTQ"."SysNumber" = "OBTN"."SysNumber" AND "OBTQ"."ItemCode" = "OBTN"."ItemCode"
WHERE "OBTQ"."ItemCode" = '{0}'
AND "OBTQ"."WhsCode" = '{1}'
AND(COALESCE("OBTQ"."Quantity",0) - COALESCE("CommitQty",0)) > 0
ORDER BY "OBTQ"."SysNumber"