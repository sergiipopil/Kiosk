export class UrlHelper {

    public static getRequestModelFromParams(params: any): any {
        let model: any = {};
        Object.keys(params).map(key => {
            if (key.indexOf('.') < 0) {
                model[key] = Number.isNaN(+params[key]) ? params[key] : +params[key];
            } else {
                let split = key.split('.');
                split.reduce((prev, curr, index) => {
                    if (!prev[curr] && index != split.length - 1) {
                        prev[curr] = {};
                    } else if (index == split.length - 1) {
                        prev[curr] = Number.isNaN(+params[key]) ? params[key] : +params[key]
                    }
                    return prev[curr];
                }, model);
            }
        });
        return model;
    }

}