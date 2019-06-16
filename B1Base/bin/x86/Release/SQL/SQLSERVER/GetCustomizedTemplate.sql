select  "Left", 
		"Top", 
		"Editable",
		"Visible"
from UIC2
inner join UICU on (UICU."TPLId" = UIC2."TPLId" and UICU."UserID" = {0})
where UIC2."FormId" = '{1}'
and UIC2."ItemId" = '{2}'