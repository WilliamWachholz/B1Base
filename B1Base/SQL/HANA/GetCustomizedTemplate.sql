select  "Left", 
		"Top", 
		"Editable",
		"Visible"
from UIC2
inner join UICU on (UICU."TPLId" = UIC2."TPLId" and UICU."UserID" = {0})
where UIC2."FormId" = '{1}'
and UIC2."ItemId" = '{2}'
union
select  "Left", 
		"Top", 
		"Editable",
		"Visible"
from UIC2
inner join UICU on (UICU."TPLId" = UIC2."TPLId")
inner join UIC3 on (UIC3."TPLId" = UICU."TPLId" and UIC3."UserID" = {0})
where UIC2."FormId" = '{1}'
and UIC2."ItemId" = '{2}'
union
select  "Left", 
		"Top", 
		"Editable",
		"Visible"
from UIC2
inner join UICU on (UICU."TPLId" = UIC2."TPLId")
inner join USR7 on (USR7."UserId" = {0})
inner join UIC6 on (UIC6."TPLId" = UICU."TPLId" and UIC6."GroupID" = USR7."GroupId)
where UIC2."FormId" = '{1}'
and UIC2."ItemId" = '{2}'