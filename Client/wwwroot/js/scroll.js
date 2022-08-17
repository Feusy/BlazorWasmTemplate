function ScrollToElement(name){
    var element = document.getElementById(name)
    
    element.scrollIntoView({ behavior: 'smooth', block: 'start'});
}