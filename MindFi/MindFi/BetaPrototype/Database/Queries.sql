select * from RequestsQueue where RequestType='FBAlbums'
select ID, Name, AlbumID, SNID, Link, AlbumType, PhotoCount from AlbumData, Entities where AlbumData.AlbumID = Entities.ID


select PostID, SNID from PostData where PostID = 43
update PostData set Created = null, Updated = null where PostID = 43

select PostID, SNID, Created, Updated from PostData where PostID = 43
update PostData set Created = '9/13/2011' where PostID = 43

select ResponseValue from RequestsQueue limit 5
select FirstName, LastName from PersonData

select count(*), State from RequestsQueue group by State

select ResponseValue from RequestsQueue where State=3 limit 5

update RequestsQueue set state=3 where state=2 and '9/18/2011'>Updated 

