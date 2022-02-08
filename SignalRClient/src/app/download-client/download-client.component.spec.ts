import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DownloadClientComponent } from './download-client.component';

describe('DownloadClientComponent', () => {
  let component: DownloadClientComponent;
  let fixture: ComponentFixture<DownloadClientComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DownloadClientComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DownloadClientComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
