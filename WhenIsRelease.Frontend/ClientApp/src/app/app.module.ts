// Imports
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { MatListModule } from '@angular/material/list';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CopyClipboardModule } from './shared/copy-clipboard.module';

// Declarations
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { ImageFormatPipe } from './imageFormatPipe';

// Providers
import { RegionService } from './services/regionservice.service';
import { PlatformService } from './services/platformservice.service';
import { SearchService } from './services/searchservice.service';
import { CalService } from './services/calservice.service';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    ImageFormatPipe
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatInputModule,
    MatIconModule,
    MatStepperModule,
    MatListModule,
    MatTabsModule,
    MatSnackBarModule,
    MatCardModule,
    MatProgressSpinnerModule,
    CopyClipboardModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' }
    ]),
    BrowserAnimationsModule
  ],
  providers: [
    RegionService,
    PlatformService,
    SearchService,
    CalService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
