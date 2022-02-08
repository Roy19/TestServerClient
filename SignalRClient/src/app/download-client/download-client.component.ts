import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

@Component({
  selector: 'app-download-client',
  templateUrl: './download-client.component.html',
  styleUrls: ['./download-client.component.css']
})
export class DownloadClientComponent implements OnInit {

  private hubConnection: HubConnection;
  fileList: any;
  selectedFile: any;
  downloadedItemString: string;
  sanitizedImageUrl: SafeResourceUrl | undefined;

  constructor(private sanitizer: DomSanitizer) {
    this.downloadedItemString = '';
    this.hubConnection = new HubConnectionBuilder().withUrl('https://127.0.0.1:7086/uploadhub').build();
  }

  ngOnInit(): void {
    this.hubConnection.start().then(() => {
      console.log('Connection established');
    }).catch(err => {
      console.log('Error occured when starting connection ' + err);
    })
  }

  getFileList(): void {
    this.hubConnection.invoke('GetFilesOnServer').then(result => {
      this.fileList = result;
    }).catch(err => {
      console.error(err);
    });
  }

  downloadUsingByteArrayData(): void {
    this.downloadedItemString = '';
    console.log('Selected file ' + this.selectedFile);
    this.hubConnection.stream('DownloadFileAsByteArray', this.selectedFile, false).subscribe({
      next: (item: any) => {
        console.log('received chunk ' + item);
        try {
          this.downloadedItemString += atob(item.toString());
        } catch (ex) {
          console.error('Failed to add item ' + ex);
        }
      },
      error: (err: any) => {
        console.error('Error occured while streaming ' + err);
      },
      complete: () => {
        this.downloadedItemString = 'data:image/jpg;base64,' + btoa(this.downloadedItemString);
        this.sanitizedImageUrl = this.downloadedItemString && this.sanitizer.bypassSecurityTrustResourceUrl(this.downloadedItemString);
        this.downloadedItemString = '';
      }
    });
  }

  downloadUsingStringData(): void {
    this.downloadedItemString = '';
    console.log('Selected file ' + this.selectedFile);
    this.hubConnection.stream('DownloadFileAsString', this.selectedFile, false).subscribe({
      next: (item: any) => {
        console.log('received chunk ' + item);
        try {
          this.downloadedItemString += atob(item.toString());
        } catch (ex) {
          console.error('Failed to add item ' + ex);
        }
      },
      error: (err: any) => {
        console.error('Error occured while streaming ' + err);
      },
      complete: () => {
        this.downloadedItemString = 'data:image/jpg;base64,' + btoa(this.downloadedItemString);
        this.sanitizedImageUrl = this.downloadedItemString && this.sanitizer.bypassSecurityTrustResourceUrl(this.downloadedItemString);
        this.downloadedItemString = '';
      }
    });
  }

}
