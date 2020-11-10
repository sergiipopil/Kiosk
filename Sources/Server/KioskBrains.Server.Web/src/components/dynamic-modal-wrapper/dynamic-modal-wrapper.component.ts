import { Component, Input, ChangeDetectorRef, ComponentRef, ViewChild, ViewContainerRef, ComponentFactory, ComponentFactoryResolver, Renderer, EventEmitter, Output } from '@angular/core';
import { BaseFrameContainerClass } from 'components/base-frame/base-frame.component';

declare var jQuery: any;

@Component({
    selector: 'dynamic-modal-wrapper',
    templateUrl: 'dynamic-modal-wrapper.component.html',
    styleUrls: ['dynamic-modal-wrapper.scss']
})
export class DynamicModalWrapper {
    @ViewChild('target', { read: ViewContainerRef }) target: ViewContainerRef;
    @ViewChild('modalContainer') container: any;
    @ViewChild('closeIcon') closeIcon: any;
    @Input() component: any;
    @Input() params: any;
    @Output() modalClose: EventEmitter<any> = new EventEmitter<any>();
    @Input() index: number = 1;
    @Input() modalConfig: any = {};


    cmpRef: ComponentRef<any>;
    private isViewInitialized: boolean = false;

    constructor(private resolver: ComponentFactoryResolver, private renderer: Renderer, private changeDetector: ChangeDetectorRef) { }

    closeModal($event) {
        this.modalClose.next(this.index);
    }

    updateComponent() {
        if (!this.isViewInitialized) {
            return;
        }

        if (this.cmpRef) {
            this.cmpRef.destroy();
        }
        
        let factory = this.resolver.resolveComponentFactory(this.component);
        this.target.clear();
        this.cmpRef = this.target.createComponent(factory);

        Object.assign(this.cmpRef.instance, this.params);
        this.renderer.setElementClass(this.container.nativeElement, "show", true);       
        this.changeDetector.detectChanges();
        if (!this.modalConfig || !this.modalConfig.hideCloseButton) {
            jQuery(this.container.nativeElement).find("." + BaseFrameContainerClass).prepend(jQuery(this.closeIcon.nativeElement));
            this.renderer.setElementClass(this.closeIcon.nativeElement, "show", true);
        }
    }

    ngOnChanges() {
        this.updateComponent();
    }

    ngAfterViewInit() {        
        this.isViewInitialized = true;
        if (this.container && this.container.nativeElement) {
            this.renderer.setElementStyle(this.container.nativeElement, "z-index", (this.index + 5000).toString());
        }
        this.updateComponent();
        
    }

    ngOnDestroy() {
        if (this.cmpRef) {
            this.cmpRef.destroy();
        }
    }
}
