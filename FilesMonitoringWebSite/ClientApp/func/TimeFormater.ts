export function FormateTime(date:Date, returnTime: boolean = true): string {
    if(returnTime){
        return date.toLocaleString();
    } else{
        return date.toLocaleDateString();
    }
    // let day = date.getDate().toString(),
    //     mounth = (date.getMonth() + 1).toString(),
    //     hours = date.getHours().toString(),
    //     minutes = date.getMinutes().toString(),
    //     seconds = date.getSeconds().toString();

    // result += day.length == 1 
    //     ? '0' + day + '/'
    //     : day + '/';

    // result += mounth.length == 1 
    //     ? '0' + mounth + '/'
    //     : mounth + '/';

    // result += date.getFullYear() + ' ';

    // if(returnTime){
    //     result += hours.length == 1
    //         ? '0' + hours + '-'
    //         : hours + '-';
        
    //     result += minutes.length == 1
    //         ? '0' + minutes + '-'
    //         : minutes + '-';
        
    //     result += seconds.length == 1
    //         ? '0' + seconds
    //         : seconds;
    // }
}