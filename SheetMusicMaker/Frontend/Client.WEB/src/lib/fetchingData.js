export async function GetPdf(id){
    let asd = await fetch(`http://localhost:5151/api/Pdf/${id}`)
    if(asd.ok){
        let json = await asd.json()
        console.log('success: ' + json)
        return json
    }
    else{
        alert('Your get request was DUMB!')
    }
}