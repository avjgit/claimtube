import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
    urlToUpload: string = 'http://hackif2017agapi.azurewebsites.net/api/';
    //urlToUpload: string = 'http://localhost:32765/api/';
    progress: number = 0;
    isLoading: boolean = false;
    loaded: boolean = false;
    hasError: boolean = false;

    videoUploaded: boolean = false;
    videoUrl: string;
    videoProcessStatus: string;
    videoProcessProgress: string;
    videoTags: string[];

    debug: boolean = true;
    imageProcessed: boolean = false;
    imageCaptions: string[];
    imageCategories: string[];
    imageTags: string[];

    clientPolicies: any[];

    constructor(
        private http: Http,
        private changeDetector: ChangeDetectorRef) { }

    ngOnInit(): void {
        this.http.get(this.urlToUpload + 'policies')
            .subscribe(result => {
                this.clientPolicies = result.json();
            },
            error => console.error(error));
    }

    onFileSelected(event: any): void {
        var files: any = event.target.files;
        if (files.length === 0) {
            this.somethingChanged();
            return;
        }

        this.videoUploaded = false;
        this.isLoading = true;
        this.progress = 0;
        this.somethingChanged();
        this.makeFileRequest(this.urlToUpload + 'video', [], files);
    }

    private loadDone(): void {
        this.isLoading = false;
        this.loaded = true;
        this.somethingChanged();
        setTimeout((): void => {
            this.loaded = false;
            this.progress = 0;
            this.hasError = false;
            this.somethingChanged();
        }, 1000);
    }

    private makeFileRequest(url: string, params: string[], files: File[]): void {
        let formData: FormData = new FormData(),
            xhr: XMLHttpRequest = new XMLHttpRequest();

        for (let i: number = 0; i < files.length; i++) {
            formData.append('uploads[]', files[i], files[i].name);
        }

        xhr.onreadystatechange = () => {
            if (xhr.readyState === 4) {

                this.progress = 100;
                if (xhr.status === 200) {
                    this.onVideoUploaded(xhr.responseText.replace(/\"/g, ''));
                    console.log(xhr.responseText === '""'
                        ? 'File uploaded successfully' : xhr.responseText.replace(/\"/g, ''));

                } else {
                    this.hasError = true;
                    let error: string;
                    let parameters: any = null;

                    if (xhr.status === 500 && xhr.response) {
                        let response: any = JSON.parse(xhr.response);
                        error = response.exceptionMessage ? response.exceptionMessage :
                            response.message ? response.message : 'Server error';
                        parameters = response.parameters ? response.parameters : null;
                    } else if (xhr.status === 401 || xhr.status === 403) {
                        error = 'Session expired. Please refresh page';
                    } else {
                        error = xhr.status ? `${xhr.status} - ${xhr.statusText}` : 'Server error';
                    }

                    console.error(error);
                }

                this.loadDone();
            }
        };

        xhr.upload.onprogress = (event) => {
            this.progress = Math.round(event.loaded / event.total * 100);
            this.somethingChanged();
        };

        xhr.open('POST', url, true);
        xhr.send(formData);
    }

    private onVideoUploaded(id: string): void {

        this.videoUploaded = true;
        this.http.get(this.urlToUpload + 'video/' + id + '/state')
            .subscribe(result => {
                let r: any = result.json();
                this.videoProcessStatus = r.state;
                this.videoTags = r.tags;

                if (r.thumbnailUrl
                    && !this.imageProcessed
                    && !r.thumbnailUrl.endsWith('0000000000')) {
                    this.videoUrl = r.thumbnailUrl;

                    this.http.get(this.urlToUpload + 'image/process_image/?url=' + encodeURIComponent(this.videoUrl))
                        .subscribe(result => {
                            let r: any = result.json();
                            this.imageProcessed = true;
                            this.imageCaptions = r.captions;
                            this.imageCategories = r.categories;
                            this.imageTags = r.tags;

                        }, error => console.error(error))
                }

                if (this.videoProcessStatus !== 'Processed') {
                    this.videoProcessProgress = r.processingProgress;
                } else {
                    this.videoProcessProgress = '100%';
                }

                if (this.videoTags.length === 0) {
                    setTimeout(() => this.onVideoUploaded(id), 3000);
                }

            }, error => console.error(error));

    }

    private somethingChanged(): void {
        this.changeDetector.markForCheck();
    }
}
