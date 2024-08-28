import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl, NgModel } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MAT_STEPPER_GLOBAL_OPTIONS } from '@angular/cdk/stepper';
import { Region, Platform, Release } from '../models';
import { RegionService, PlatformService, SearchService, CalService } from '../services';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  providers: [{
    provide: MAT_STEPPER_GLOBAL_OPTIONS, useValue: { displayDefaultIndicatorType: false }
  }],
  animations: [
    trigger('slideInOut', [
      transition(':enter', [
        style({ transform: 'translateX(100%)' }),
        animate('200ms ease-in', style({ transform: 'translateX(0%)' }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateX(100%)' }))
      ])
    ])
  ]
})

export class HomeComponent implements OnInit {
  loadingReleases: boolean = false;
  releases: Release[];
  platforms: Platform[];
  allplatforms: Platform[];
  regions: Region[];
  searchQuery: string = '';
  searchRegions: Region[] = [];
  searchRegionList = new FormControl();
  searchPlatforms: Platform[] = [];
  searchPlatformList = new FormControl();
  calendarFormGroup: FormGroup;
  calendarRegions: Region[] = [];
  calendarRegionList = new FormControl();
  calendarPlatforms: Platform[] = [];
  calendarPlatformList = new FormControl();
  calendarUrl: string;

  constructor(private regionService: RegionService, private platformService: PlatformService, private searchService: SearchService, private calService: CalService, private _formBuilder: FormBuilder, private _snackBar: MatSnackBar) { }

  ngOnInit() {
    this.regionService.getRegions().subscribe(r => this.regions = r);
    this.platformService.getAllPlatforms().subscribe(p => this.allplatforms = p);
    this.platformService.getPlatforms().subscribe(p => this.platforms = p);
    this.calendarFormGroup = this._formBuilder.group({
      calendarCtrl: ['', Validators.required]
    });
  }

  public notify(): void {
    this._snackBar.open('Copied to clipboard', 'close', {
      duration: 3000,
    });
  }

  selectAll(select: NgModel, values) {
    select.update.emit(values);
  }

  deselectAll(select: NgModel) {
    select.update.emit([]);
  }

  Search() {
    this.loadingReleases = true;
    this.searchService.search(this.searchQuery, this.searchRegions, this.searchPlatforms).subscribe(r => {
      this.releases = r;
      this.loadingReleases = false;
    });
  }

  GenerateCalendarLink() {
    this.calendarUrl = this.calService.getCalendarLink(this.calendarRegions, this.calendarPlatforms);
  }

  GenerateCalendar() {
    this.calService.getCalendar(this.calendarRegions, this.calendarPlatforms).subscribe(data => this.downloadFile(data));
  }

  downloadFile(data: any) {
    const blob = new Blob([data], { type: 'text/calendar' });
    const url = window.URL.createObjectURL(blob);
    window.open(url);
  }
}
