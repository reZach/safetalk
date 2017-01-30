import { Component } from "../node_modules/@angular/core"
//import { bootstrap } from "../node_modules/@angular/platform-browser-dynamic"

@Component({
    selector: "hello-angular",
    template: "<h1>{{greeting}}</h1>"
})
class HelloAngularComponent {
    greeting: string;
    constructor() {
        this.greeting = "Hello Angular 2!";
    }
}