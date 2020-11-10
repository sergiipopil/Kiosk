import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ModalGuard } from './routeGuards/modal.guard';
import { AuthGuard } from './routeGuards/auth.guard';
import { routes } from 'app/route.config';

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule],
    providers: [AuthGuard, ModalGuard]
})
export class AppRoutingModule {
}
